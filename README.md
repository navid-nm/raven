# Raven

Language that compiles to JS. Specifically for simplifying JS-related code, making it more concise, typesafe and expressive.

## Install

-  Can be installed manually by downloading the exe from the GitHub release, and placing it in a folder, then adding that folder to system PATH.

-  Can also be installed via npm, yarn, etc: `npm install -g ravenlang`

## Examples

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

---

Types:

```d
|| cn       -> Connection
|| video    -> Video
|| err      -> Error
cn.on("ReceiveVideo", fn(video)
{
    try {
        say(
            "Video received",
            video.str()
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

Raven supports templating, which allows for importing HTML snippets and using them directly in JS code.
This makes it easier to manage and reuse HTML structures.

main.rn:

```d
import user

onready(fn() {
    val User = {
        name: "Jane Doe",
        avatar: "avatar.jpg",
        bio: "Person"
    }
    let ProfileContainer = doc.get("profile-container")
    if (ProfileContainer) {
        val ProfileCard = doc.make("div")
        ProfileCard.className = ProfileClass
        ProfileCard.innerHTML = ProfileTemplate(User)
        ProfileContainer.put(ProfileCard)
    }
})
```

Use the .rnm extension for importable modules.

It indicates to the transpiler to include the module content in the program that is using it (in this case main.rn).

This way 1 .js is generated (main.js), rather than 2 (main.js and user.js).

user.rnm:

```d
val ProfileClass = "profile-card"
val ProfileTemplate = rhtml("templates/profile.rhtml")
```

templates/profile.rhtml:

```html
<div class="profile">
   <img src="${User.avatar}" alt="${User.name}" />
   <h2>${User.name}</h2>
   <p>${User.bio}</p>
</div>
```

This transpiles to main.js:

```
const ProfileClass = "profile-card";
const ProfileTemplate = function(User) {
    return `
        <div class="profile">
            <img src="${User.avatar}" alt="${User.name}">
            <h2>${User.name}</h2>
            <p>${User.bio}</p>
        </div>
    `;
};

document.addEventListener("DOMContentLoaded", function() {
    const User = {
        name: "Jane Doe",
        avatar: "avatar.jpg",
        bio: "Person"
    };
    const ProfileContainer = document.getElementById("profile-container");
    if (ProfileContainer) {
        const ProfileCard = document.createElement("div");
        ProfileCard.className = ProfileClass;
        ProfileCard.innerHTML = ProfileTemplate(User);
        ProfileContainer.appendChild(ProfileCard);
    }
});
```

---
