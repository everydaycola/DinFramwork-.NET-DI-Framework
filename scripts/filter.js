addEventListener("load", filter);

function filter() {
    // get filter from url
    const url = new URL(window.location);
    const searchParams = new URLSearchParams(url.search);
    const filter = searchParams.get("filter");
    // Get all elements to potentially filter
    const elements = document.querySelectorAll("[cathegory]");
    // if there is no filter is present, make everything visible
    if (filter) {
        // Loop through each element
        for (let element of elements) {

            // Check if category matches the filter
            if (element.getAttribute("cathegory") === filter) {
                // Show the element if it matches the filter
                element.classList.remove("invis");
            } else {
                // Hide the element if it doesn't match the filter
                element.classList.add("invis");
            }
        }
    } else {
        // Loop through each element and make invis
        for (let element of elements) {
            element.classList.remove("invis");
        }
    }
}