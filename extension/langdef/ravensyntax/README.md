# Raven

Syntax highlighter, formatter, and linter for Raven language.

Raven is a simple transpiler that affords syntactic sugar over JS, aiming to make JS less ugly without adding anything else.

https://github.com/navid-nm/raven

---

Simple example:

```
fn main() {
    say("test")
}
```

Transpiles to the following JavaScript:

```
function main() {
    console.log("test");
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
