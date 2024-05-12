
addEventListener("load", init);

function init() {
    let form = document.getElementById("form");
    form.addEventListener("submit", loginSubmit, false);
}

function loginSubmit(event) {
    let message = "";
    if (!validateFirstName()) {
        message = "Naam niet (correct) ingevuld!";
    }
    if (!validateCvv()) {
        message = "Cvv niet (correct) ingevuld!";
    }
    if (!validateTerms()) {
        message = "U moet akkoord gaan met de Terms en condities";
    }
    if (!validateDate()) {
        message = "Datum is niet mogelijk"
    }
    if (message.length > 0) {
        document.getElementById("feedback").innerHTML = message;
        event.preventDefault();
    }
}

document.getElementById('fname').addEventListener('blur', validateFirstName);
document.getElementById('date').addEventListener('blur', validateDate);
document.getElementById('cvv').addEventListener('blur', validateCvv);
document.getElementById('terms').addEventListener('change', validateTerms);

// Validation functions
function validateFirstName() {
    const firstNameInput = document.getElementById('fname');
    const firstName = firstNameInput.value.trim();
    if (firstName.length < 2) {
        firstNameInput.style.backgroundColor = 'red';
        return false;
    } else {
        firstNameInput.style.backgroundColor = 'white';
        return true;
    }
}

function validateDate() {
    const today = Date.now();
    const selectedDate = new Date(document.getElementById('date').value);

    // Check if selected date is after today's date (ignoring time)
    return selectedDate > today;
}


function validateCvv() {
    const cvvInput = document.getElementById('cvv');
    const cvv = cvvInput.value.trim();
    const regex = /^\d{3}$/;
    if (!regex.test(cvv)) {
        cvvInput.style.backgroundColor = 'red';
        return false;
    } else {
        cvvInput.style.backgroundColor = 'white';
        return true;
    }
}

function validateTerms() {
    const termsCheckbox = document.getElementById('terms');
    if (!termsCheckbox.checked) {
        termsCheckbox.style.backgroundColor = 'red';
        return false;
    } else {
        termsCheckbox.style.backgroundColor = 'white';
        return true;
    }
}

function validateForm() {
    // Check if all checked fields are valid
    return validateFirstName()  && validateCvv() && validateTerms() && validateDate();
}