#!/usr/bin/env node

/*
 * Minimal smoke test for a deployed or local C# Playground.
 *
 * Usage:
 *   node scripts/smoke-test.js https://csharp-roslyn-wasm-playground.pages.dev/
 *
 * Requires Playwright and a Chromium-based browser:
 *   npm install playwright
 */

const { chromium } = require('playwright');

const baseUrl = (process.argv[2] || 'http://127.0.0.1:5158/').replace(/\/?$/, '/');

async function main() {
    const browser = await chromium.launch({ channel: 'msedge', headless: true });
    const page = await browser.newPage();
    const failures = [];

    page.on('pageerror', error => failures.push(`pageerror: ${error.message}`));
    page.on('requestfailed', request => {
        failures.push(`requestfailed: ${request.url()} (${request.failure()?.errorText})`);
    });

    try {
        await page.goto(baseUrl, { waitUntil: 'domcontentloaded', timeout: 120000 });
        await page.locator('#code-editor').waitFor({ timeout: 120000 });

        await page.locator('#code-editor').fill('Console.WriteLine("smoke test");');
        await page.locator('.compile-btn').click();
        await page.waitForFunction(() => {
            return document.querySelector('#output')?.textContent?.includes('smoke test');
        }, { timeout: 90000 });

        const apiOutput = await page.evaluate(() => {
            return window.playgroundHost.run('Console.WriteLine("postmessage smoke");');
        });
        if (!apiOutput.includes('postmessage smoke')) {
            failures.push(`postMessage output mismatch: ${JSON.stringify(apiOutput)}`);
        }

        const snippetCount = await page.evaluate(() => {
            const editor = document.getElementById('code-editor');
            editor.value = 'for';
            editor.dispatchEvent(new Event('input', { bubbles: true }));
            return new Promise(resolve => {
                setTimeout(() => {
                    resolve(document.querySelectorAll('.code-completion-item').length);
                }, 350);
            });
        });
        if (snippetCount < 1) {
            failures.push('snippet completion menu did not open');
        }

        if (failures.length > 0) {
            console.error('SMOKE TEST FAILED');
            for (const failure of failures) console.error(`- ${failure}`);
            process.exitCode = 1;
        }
        else {
            console.log('SMOKE TEST PASSED');
        }
    }
    finally {
        await browser.close();
    }
}

main().catch(error => {
    console.error(error);
    process.exit(1);
});
