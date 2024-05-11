
addEventListener("load", init);

function init() {

    const url = new URL(window.location);
    const searchParams = new URLSearchParams(url.search);


    const id = searchParams.get("id");

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
        }
    }

    console.log("huh")

    const fullName = searchParams.get("firstname")
    const email = searchParams.get("email");
    const phoneNumber = searchParams.get("tel");
    const address = searchParams.get("street and number");
    const time = searchParams.get("date");
    const numberOfPeople = searchParams.get("aantal");
    const extraInfo = searchParams.get("extra informatie");

    document.getElementById("fname").textContent += fullName;
    document.getElementById("femail").textContent += email;
    document.getElementById("ftel").textContent += phoneNumber;
    document.getElementById("address").textContent += address;
    document.getElementById("time").textContent += time;
    document.getElementById("numberOfPeople").textContent += numberOfPeople;
    document.getElementById("extraInfo").textContent += extraInfo


}