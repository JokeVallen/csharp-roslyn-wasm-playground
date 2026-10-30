(function () {
    let worker = null;
    let nextRequestId = 0;
    const pending = new Map();

    function getWorker() {
        if (worker) return worker;

        worker = new Worker(new URL('worker.js', document.baseURI), { type: 'module' });
        worker.addEventListener('message', (event) => {
            const { id, result, error } = event.data || {};
            const request = pending.get(id);
            if (!request) return;

            pending.delete(id);
            if (error) {
                request.reject(new Error(error));
                // Startup failures leave the worker without exports. Drop it so
                // the next request starts a clean worker instead of failing forever.
                if (worker) {
                    worker.terminate();
                    worker = null;
                }
            }
            else {
                request.resolve(result);
            }
        });

        worker.addEventListener('error', (event) => {
            for (const request of pending.values()) {
                request.reject(new Error(event.message || 'Roslyn worker failed.'));
            }
            pending.clear();
            if (worker) {
                worker.terminate();
                worker = null;
            }
        });

        return worker;
    }

    function request(command, payload) {
        return new Promise((resolve, reject) => {
            const id = ++nextRequestId;
            pending.set(id, { resolve, reject });
            getWorker().postMessage({ id, command, payload });
        });
    }

    function terminate() {
        if (worker) {
            worker.terminate();
            worker = null;
        }

        for (const request of pending.values()) {
            request.reject(new Error('Roslyn worker was stopped.'));
        }
        pending.clear();
    }

    window.roslynWorker = {
        complete: (code, position, targetFramework) =>
            request('complete', { code, position, targetFramework }),
        compile: (code, targetFramework) =>
            request('compile', { code, targetFramework }),
        format: (code) =>
            request('format', { code }),
        execute: (assemblyBase64, inputsJson) =>
            request('execute', { assemblyBase64, inputsJson }),
        terminate
    };
})();
