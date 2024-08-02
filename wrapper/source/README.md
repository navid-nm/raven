# Raven

Transpiler that compiles Raven to JS.

Raven is a language focused on clean syntax, expressiveness, and simplicity.

Site: https://ravenlang.pages.dev

Git: https://github.com/navid-nm/raven

VSCode Extension: https://marketplace.visualstudio.com/items?itemName=NavidM.ravenlang

---

## Usage

## Examples

Simple example:

```
fn add(x, y) = x + y

fn main() {
    say(add(1, 2))
}
```

Transpiles to the following JavaScript:

```
"use strict";

function add(x, y) {
    return x + y;
}

function main() {
    console.log(add(1, 2));
}
```

Types:

```
|| cn       -> Connection
|| video    -> Video
|| err      -> Error
cn.on("ReceiveVideo", fn(video)
{
    try {
        say(
            "Video received",
            video
        );
    } die (err) {
        warn(
            "Error appending video card:",
            err
        )
    }
});
```

---

##### Notes

-  Includes builtin HTML templating via .rhtml files and the rhtml() builtin.
-  Has inline JS capability if really required

---
