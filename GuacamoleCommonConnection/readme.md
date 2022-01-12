

Began translating Guacamole-common-js into C#



My initial approach was to emulate a Guacamole-client and then only read clipboard related data.

This approach has two main issues:

- It requires me to translate a whole project from Javascript (not even Typescript!) into C#
- As it turns out, it's not possible to connect with multiple Guacamole clients at once, thus only allowing a tunnel when not connected with browser.

When I realised this, I changed approach to making a userscript to mirror the websocket to an local application.