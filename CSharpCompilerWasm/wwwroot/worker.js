import { dotnet } from './_framework/dotnet.js';

// GitHub Pages occasionally resets HTTP/2 streams while the worker is
// downloading the .NET assembly set. Retry transient fetch failures instead
// of leaving the worker permanently stuck at startup.
const nativeFetch = self.fetch.bind(self);
const retryDelays = [150, 350, 800];
self.fetch = async function (input, init) {
    let lastError;
    for (let attempt = 0; attempt <= retryDelays.length; attempt++) {
        try {
            const response = await nativeFetch(input, init);
            if (response.ok || response.status < 500 || attempt === retryDelays.length) {
                return response;
            }
            lastError = new Error(`HTTP ${response.status} for ${String(input)}`);
        }
        catch (error) {
            lastError = error;
        }

        if (attempt < retryDelays.length) {
            await new Promise(resolve => setTimeout(resolve, retryDelays[attempt]));
        }
    }
    throw lastError;
};

let assemblyExports = null;
let startupError = undefined;
let ready = false;
const pendingMessages = [];

async function handleMessage(event) {
    const { id, command, payload } = event.data || {};
    try {
        if (!assemblyExports) {
            throw new Error(startupError || 'Roslyn worker exports were not loaded.');
        }

        let result;
        if (command === 'complete') {
            result = assemblyExports.RoslynWorker.GetCompletions(
                payload.code,
                payload.position,
                payload.targetFramework);
        }
        else if (command === 'compile') {
            result = assemblyExports.RoslynWorker.Compile(
                payload.code,
                payload.targetFramework);
        }
        else if (command === 'warmup') {
            result = true;
        }
        else if (command === 'format') {
            result = assemblyExports.RoslynWorker.Format(payload.code);
        }
        else if (command === 'execute') {
            result = assemblyExports.RoslynWorker.Execute(
                payload.assemblyBase64,
                payload.inputsJson);
        }
        else {
            throw new Error(`Unknown worker command: ${command}`);
        }

        self.postMessage({ id, result });
    }
    catch (error) {
        self.postMessage({ id, error: error?.message || String(error) });
    }
}

// Register the listener before awaiting dotnet.create(). A module worker can
// dispatch queued messages while the top-level await is still pending, so
// attaching the listener afterwards can lose the first request.
self.addEventListener('message', (event) => {
    if (!ready) {
        pendingMessages.push(event);
        return;
    }

    handleMessage(event);
});

try {
    console.log('Roslyn worker: booting .NET runtime');
    const { getAssemblyExports, getConfig } = await dotnet.create();
    const config = getConfig();
    console.log('Roslyn worker: loading exports for', config.mainAssemblyName);
    assemblyExports = await getAssemblyExports(config.mainAssemblyName);
    console.log('Roslyn worker: exports ready');
}
catch (error) {
    startupError = error?.message || String(error);
    console.error('Roslyn worker startup failed:', startupError);
}

ready = true;
for (const event of pendingMessages.splice(0)) {
    await handleMessage(event);
}