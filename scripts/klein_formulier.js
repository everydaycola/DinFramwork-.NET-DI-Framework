
addEventListener("load", init)

function init() {
    const url = new URL(window.location);
    const searchParams = new URLSearchParams(url.search);

    const naam = searchParams.get("naam");
    let name = document.getElementById("fname");
    name.value = naam;

    const tel = searchParams.get("tel");
    let tell = document.getElementById("ftel");
    tell.value = tel;

    const email = searchParams.get("email");
    let mail = document.getElementById("femail");
    mail.value = email;

    console.log(naam);
    console.log(tel);
    console.log(email);

}