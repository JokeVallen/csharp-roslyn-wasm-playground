(function () {
    const originalFetch = window.fetch.bind(window);
    const retryDelays = [150, 350, 800];

    window.fetch = async function (input, init) {
        let lastError;

        for (let attempt = 0; attempt <= retryDelays.length; attempt++) {
            try {
                const response = await originalFetch(input, init);
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
})();
