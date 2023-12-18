
const chBoxes = document.querySelectorAll('.dropdown-menu input[name="subFunction"]');
const chRoleBoxes = document.querySelectorAll('.dropdown-menu input[name="Role"]');
const dpBtn = document.getElementById('multiSelectDropdown');
const dpRole = document.getElementById('Role');
let mySelectedListItems = [];
let mySelectedListItemsRole = [];

function handleCB() {
    mySelectedListItems = [];
    let mySelectedListItemsText = '';

    chBoxes.forEach((checkbox) => {
        if (checkbox.checked) {
            let checkboxvalue = "S" + checkbox.value;
            let checkboxText = $(`label[id="${checkboxvalue}"]`).text();
            mySelectedListItems.push(checkbox.value);
            mySelectedListItemsText += checkboxText.trim() + ', ';
        }
    });

    dpBtn.innerText =
        mySelectedListItems.length > 0
            ? mySelectedListItemsText.slice(0, -2) : 'Select';
}
function handleRoleCB() {
    mySelectedListItemsRole = [];
    let mySelectedListItemsTextRole = '';
    chRoleBoxes.forEach((checkbox) => {
        if (checkbox.checked) {
            let checkboxText = $(`label[for="${checkbox.value}"]`).text();
            mySelectedListItemsRole.push(checkbox.value);
            mySelectedListItemsTextRole += checkboxText.trim() + ', ';
        }
    });

    dpRole.innerText =
        mySelectedListItemsRole.length > 0
            ? mySelectedListItemsTextRole.slice(0, -2) : 'Select';
}

chBoxes.forEach((checkbox) => {
    checkbox.addEventListener('change', handleCB);
});
chRoleBoxes.forEach((checkbox) => {
    checkbox.addEventListener('change', handleRoleCB);
});
// Function to validate the form data
function validateForm(form) {
    var isValid = true;

    $(':input[required]:visible', form).each(function () {
        if ($(this).val().trim() === '') {
            isValid = false;
            $(this).css('border', '1px solid red');
        } else {
            $(this).css('border', '');
        }
    });

    if (!isValid) {
        alert('Please fill all required fields.');
    }
    return isValid;
}
// Function to validate email
function validateEmailField() {
    // Get email field value
    var email = document.getElementById('email').value;

    // Check if email field is empty
    if (email.trim() === '') {
        alert('Please enter email to search user.');
        return false;
    }

    // Regular expression for basic email validation
    var re = /[\w-]+(\.[\w-]+)*@[\w-]+(\.[\w-]+)*(\.[a-zA-Z]{2,})$/;

    // Check if email is correctly formatted
    if (!re.test(String(email).toLowerCase())) {
        alert('Email is not valid, please enter a valid email.');
        return false;
    }

    return true;
}
// EMail autocomplete
$('#email').on('keyup', function () {
    var email = $(this).val();
    if (email == "") {
        $('#emailSuggestions').empty();
    }

    // Only make the AJAX call if at least 3 characters have been entered
    if (email.length >= 3) {
        $.ajax({
            url: '/User/GetEmailSuggestions',
            type: 'GET',
            data: { 'email': email },
            success: function (result) {
                // clear old suggestions
                $('#emailSuggestions').empty();
                $('#emailSuggestions').show();

                // loop through result and append to suggestions div
                $.each(result, function (key, value) {
                    $('#emailSuggestions').append('<div class="suggestion">' + value + '</div>');
                });
            },
            error: function (xhr, status, error) {
                console.error('An error occurred: ', error);
            }
        });
    }
});

// add click event to all suggestion divs
$(document).on('click', '.suggestion', function () {
    var selectedEmail = $(this).text();
    $('#email').val(selectedEmail);
    $('#emailSuggestions').empty();
});
// hide suggestions div when input loses focus, after a short delay
var timeoutId;
$('#email').on('blur', function () {
    timeoutId = setTimeout(function () {
        $('#emailSuggestions').hide();
    }, 500); // 500 ms delay
});

$(document).on('mousedown', '.suggestion', function () {
    clearTimeout(timeoutId);
});
$(document).ready(function () {
    handleCB();
    handleRoleCB();
    $("#btnSearch").click(function (event) {
        event.preventDefault();
        var userEmailId = document.getElementById('email').value;

        if (!validateEmailField()) {
            // If validation fails, prevent form from submitting
            event.preventDefault();
        }
        else {
            var form = $('#Userform');
            $('#loader').show();
            $.ajax({
                url: '/User/GetUserInfo',
                data: { userEmailId: userEmailId },
                type: 'GET',
                dataType: 'json',
                success: function (data) {
                    $('#txtUserIdForNewUser').val(data.userId);
                    $('#UserName').val(data.userName);
                    if (data.errorMsg != "" && data.errorMsg != null) {
                        $('#loader').hide();
                        alert(data.errorMsg);
                        return false;
                    }
                    $('#loader').hide();
                    $('#userOtherDetailsDiv').show();

                },
                error: function (error) {
                    console.log('Error: ', error);
                }
            });

        }


    });
    $("#btnSave").click(function (event) {
        event.preventDefault();
        var isValid = true;
        let temp = $('#Userform');
        var form = $('#Userform').serialize();
        if ($('input[name="Role"]:checked').length === 0) {
            isValid = false;
            $('#roleSpan').text('Please select at least one role.');
        }
        else {
            $('#roleSpan').text('');
        }
        if ($('input[name="subFunction"]:checked').length === 0) {
            isValid = false;
            $('#subFunctionSpan').text('Please select at least one sub function.');
        }
        else {
            $('#subFunctionSpan').text('');
        }
        if ($('#activeFlagID').val() === '') {
            isValid = false;
            $('#activeSpan').text('Please select status.');
        }
        else {
            $('#activeSpan').text('');
        }
        if (isValid) {

            $.ajax({
                url: '/User/SaveUser',
                data: form,
                type: 'POST',
                success: function (data) {
                    alert(data);
                    window.location.href = '/User/ManageUser'
                },
                error: function (error) {
                    console.log('Error: ', error);
                }
            });
        }

    });
});

