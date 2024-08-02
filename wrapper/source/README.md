# Raven

Transpiler that compiles Raven to JS.

Raven is a language focused on clean syntax, expressiveness, and simplicity.

Site: https://ravenlang.pages.dev

Git: https://github.com/navid-nm/raven

VSCode Extension: https://marketplace.visualstudio.com/items?itemName=NavidM.ravenlang

---

## Installation and Usage

`npm install -g raven`

Run "raven" in commandline without any args to compile all .rn files in current dir and containing subdirs to .js.

To run .rn files directly run "raven -r file.rn".

## Examples

Simple example:

```
fn add(x, y) = x + y

fn main() {
    val result = add(1, 2)
    if (result == 3) {
        say("will always print")
    }
}
```

Transpiles to the following JS:

```
"use strict";

function add(x, y) {
   return x + y;
}

function main() {
   const result = add(1, 2);
   if (result === 3) {
      console.log("will always print");
   }
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
