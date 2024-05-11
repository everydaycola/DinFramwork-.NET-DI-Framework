
addEventListener("load", init);

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

    const id = searchParams.get("id");
    let ids = document.getElementById("id");
    ids.value = id;


    for(let item of window.data) {
        if (item.id == id) {

            console.log(item.naam);

            document.getElementById("title").innerHTML = item.naam;
            document.getElementById("price").innerHTML = new Intl.NumberFormat('de-DE', { style: 'currency', currency: 'EUR' }).format(
                item.price,
            );
            document.getElementById("energy").innerHTML = item.energie;
            document.getElementById("heating").innerHTML = item.heating;
            document.getElementById("locatie").innerHTML = item.locatie;
            document.getElementById("oppervlakte").innerHTML = item.oppervlakte;
            document.getElementById("kamers").innerHTML = item.aantal.kamers + " kamers";
            document.getElementById("slaapkamers").innerHTML = item.aantal.slaapkamers + " slaapkamers";
            document.getElementById("badkamers").innerHTML = item.aantal.badkamers + " badkamers";


            imagesSection = document.getElementById("images");

            console.log(item.images.length);

            for (let i = 0; i < item.images.length; i++) {
                img = imagesSection.appendChild(document.createElement("figure")).appendChild(document.createElement("img"));
                img.alt = item.altImages[i];
                img.src = item.images[i];
            }



            for (let i = 0; i < 3; i++) {
                let paragraph = document.getElementById("p" + (i+1).toString());
                paragraph.querySelector("h4").textContent = item.paragraphs.titles[i];
                paragraph.querySelector("p").textContent = item.paragraphs.text[i];
            }
        }
    }



}