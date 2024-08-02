# Raven

Raven is a transpiler that affords syntactic sugar over JS.

https://github.com/navid-nm/raven

---

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

Typehints:

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
