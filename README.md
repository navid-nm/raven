# Raven

Raven is a simple transpiler that affords syntactic sugar over JS, aiming to make JS less ugly without adding anything else.

---

Examples:

```
fn main() {
    print("test")
}
```

Transpiles to the following JavaScript:

```
function main() {
    console.log("test");
}

```

Raven supports templating, which allows for importing HTML snippets and using them directly in JS code.
This makes it easier to manage and reuse HTML structures.

main.rn:

```
import user_profile

doc.listen(&ready, fn() {
    const User = { name: "Jane Doe", avatar: "avatar.jpg", bio: "Person" };
    const ProfileContainer = doc.get("profile-container");
    if (ProfileContainer) {
        const ProfileCard = doc.make("div");
        ProfileCard.ClassName = ProfileClass;
        ProfileCard.InnerHTML = ProfileTemplate(User);
        ProfileContainer.AddSub(ProfileCard);
    }
});
```

user_profile.rn:

```
const ProfileClass = "profile-card";
const ProfileTemplate = rhtml("templates/profile.rhtml");
```

templates/profile.rhtml:

```
<div class="profile">
    <img src="${User.avatar}" alt="${User.name}">
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
    const User = { name: "John Doe", avatar: "avatar.jpg", bio: "Developer" };
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
