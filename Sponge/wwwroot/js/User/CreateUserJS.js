
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
    getSuggestions('email');
});

$('#UserName').on('keyup', function () {
    getSuggestions('UserName');
});
$('#email').on('blur', function () {
    let email = $(this).val();
    if (email == '') {
        $('#email').val('');
        $('#UserName').val('');
        $('#txtUserIdForNewUser').val('');
        $('#userOtherDetailsDiv').hide();
        $('#emailSuggestions').hide();
    }
});
$('#UserName').on('blur', function () {
    let userName = $(this).val();
    if (userName == '') {
        $('#email').val('');
        $('#UserName').val('');
        $('#txtUserIdForNewUser').val('');
        $('#userOtherDetailsDiv').hide();
        $('#UserNameSuggestions').hide();
    }
});


function getSuggestions(id) {
    var enteredValue = $('#' + id).val();
    var queryurl = '';
    var timerId = null;

    // Determine API endpoint based on input id
    if (id == "email") {
        queryurl = '/User/GetEmailSuggestions';
    } else if (id == "UserName") {
        queryurl = '/User/GetUserNameSuggestions';
    } else {
        console.error('Invalid ID');
        return;
    }

    if (enteredValue == "") {
        $('#' + id + 'Suggestions').hide();
    }
    

    if (enteredValue.length >= 3) {
        $('#' + id + 'Suggestions').empty();
        $('#' + id + 'Suggestions').show();
        $('#' + id + 'Suggestions').append('<div class="suggestion">Searching...</div>');
        $.ajax({
            url: queryurl,
            type: 'GET',
            data: { [id]: enteredValue },
            success: function (result) {
                if ($('#' + id).val() != '') { // check if the input field is not empty 
                    $('#' + id + 'Suggestions').empty();
                    $('#' + id + 'Suggestions').show();
                    if (result.length !== 0) { // check if we have suggestions
                        $.each(result, function (key, value) {
                            $('#' + id + 'Suggestions').append('<div class="suggestion">' + value + '</div>');
                        });
                    }
                    else { // in case there are no suggestions
                        $('#' + id + 'Suggestions').append('<div class="suggestion">No results</div>');
                    }
                } else {
                    $('#' + id + 'Suggestions').hide();
                }
            },
            error: function (xhr, status, error) {
                console.error('An error occurred: ', error);
            }
        });
    }
}
$(document).on('click', '.suggestion', function () {
    var selectedValue = $(this).text();
    // find the parent container id
    var parentContainerId = $(this).parent().attr('id');
    // remove 'Suggestions' from the id to get the input field id
    var inputFieldId = parentContainerId.replace('Suggestions', '');
    $('#' + inputFieldId).val(selectedValue);
    $('#' + parentContainerId).empty();
    $('#' + parentContainerId).hide();
});
$(document).ready(function () {
    handleCB();
    handleRoleCB();
    //debugger;
    $("#btnSearch").on("click", function (e) {
        e.preventDefault();
        // get inputs
        var email = $("#email").val().trim();
        var username = $("#UserName").val().trim();
        if (email == "" && username == "") {
            alert('Please enter at least one field to search.');
        }
        //check if both fields not empty
        else if (email != "" && username != "") {
            alert('Please enter only one field to search');
            $("#email").val('');
            $("#UserName").val('');
        }
        else if (email != "") {
            if (!validateEmailField()) {
                // If validation fails, prevent form from submitting
                event.preventDefault();
            }
            else {
                var form = $('#Userform');
                $('#loader').show();
                $.ajax({
                    url: '/User/GetUserInfo',
                    data: { userEmailId: email },
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
        }
        else if (username != "") {
            var form = $('#Userform');
            $('#loader').show();
            $.ajax({
                url: '/User/GetUserInfoByName',
                data: { userName: username },
                type: 'GET',
                dataType: 'json',
                success: function (data) {
                    $('#txtUserIdForNewUser').val(data.userId);
                    $('#email').val(data.userEmail);
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
        if ($('#email').val() == '' || $('#UserName').val() == '') {
            alert('Please enter both email and user name.');
            event.preventDefault();
        }
        else {

            if ($('#UserName').val() == '') {
                event.preventDefault();
            }
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

        }


    });

});
