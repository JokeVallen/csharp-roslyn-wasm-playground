# Security Policy

[简体中文](SECURITY.zh-CN.md) | **English**

## Supported Versions

This project is maintained on the `main` branch and is intended for educational use. It is not a production-grade sandbox and does not receive security support as a hosted service.

Because all user code runs in the visitor's browser with the permissions of the page, only run code you trust. Do not embed the playground in a context where untrusted code could access sensitive data or authenticated actions.

## Reporting a Vulnerability

Please do **not** disclose security vulnerabilities in a public issue.

Preferred method: open a private report through GitHub's **Security → Advisories → Report a vulnerability** flow for:

https://github.com/JokeVallen/csharp-roslyn-wasm-playground

If private advisories are unavailable, contact the maintainer through the repository profile and include:

- a short description of the issue;
- steps to reproduce;
- affected browser / commit;
- potential impact;
- any suggested fix.

We will acknowledge the report and coordinate disclosure on a best-effort basis. This is a community project, so no response SLA is guaranteed.
