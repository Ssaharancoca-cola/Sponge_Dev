$(document).ready(function () {
    // Initialize all popovers
    $('[data-toggle="popover"]').popover();

    // Toggle the popover on click event of the icon
    $('[data-toggle="popover"]').on('click', function (e) {
        e.preventDefault();  // Prevent default behavior of the element
        $('[data-toggle="popover"]').not(this).popover('hide'); // Hide other popovers
        $(this).popover('toggle'); // Show or hide the current popover
    });

    // Close popovers when clicking anywhere else on the page
    $(document).on('click', function (e) {
        // Check if the clicked area is not a popover or its trigger
        if (!$(e.target).closest('.popover').length && !$(e.target).is('[data-toggle="popover"]')) {
            $('[data-toggle="popover"]').popover('hide');
        }
    });
    $("#Frequency").change(function () {
        var selectedFrequency = $(this).val();
        if (selectedFrequency) {
            $("#ForTime").prop("disabled", false);
            $.ajax({
                //url: '@Url.Action("BindGranularity", "SubjectArea")',
                url: '/SubjectArea/BindGranularity',
                type: 'GET',
                data: { frequency: selectedFrequency },
                success: function (data) {
                    //debugger;
                    var options = '';
                    $.each(data, function (i, item) {
                        options += '<option value="' + item.value + '">' + item.text + '</option>';
                    });
                    $('#ForTime').html(options);
                }
            });
            if ($("#Version").is(":checked")) {
                $.ajax({
                    //url: '@Url.Action("BindTimeLevel", "SubjectArea")',
                    url: '/SubjectArea/BindTimeLevel',
                    type: 'GET',
                    data: { frequency: selectedFrequency },
                    success: function (data) {
                        var options = '';
                        $.each(data, function (i, item) {
                            options += '<option value="' + item.value + '">' + item.text + '</option>';
                        });
                        $('#OnTime').html(options);
                    }
                });
            }
        } else {
            $("#ForTime").prop("disabled", true);
        }
    });

    $("#Version").change(function () {
        if ($(this).is(":checked")) {
            $("#OnTimeDiv").show();
            var selectedFrequency = $("#Frequency").val();
            if (selectedFrequency) {
                $.ajax({
                    //url: '@Url.Action("BindTimeLevel", "SubjectArea")',
                    url: '/Subjectarea/BindTimeLevel',
                    type: 'GET',
                    data: { frequency: selectedFrequency },
                    success: function (data) {

                        var options = '';
                        $.each(data, function (i, item) {
                            options += '<option value="' + item.value + '">' + item.text + '</option>';
                        });
                        $('#OnTime').html(options);
                    }
                });
            }
        } else {
            $("#OnTimeDiv").hide();
        }
    });
    $('#validateFormBtnID').on('click', function () {
        let subjectForm = document.getElementById('createSubjectFormID');
        if (validateForm(subjectForm))
        {
            $('#saveFormData').show()
        }

    });
});
function saveSubjectArea(event) {
    event.preventDefault();
    $.ajax({
        url: '/SubjectArea/SaveSubjectArea', 
        type: 'POST',
        data: $('#createSubjectFormID').serialize(),
        success: function (response) {
            if (response.success) {
                alert(response.message);
                  window.location.href = '/SubjectArea/ManageSubjectArea';
            } else {
                alert(response.message);
                // Handle failure case
            }
        },
        error: function () {
            alert('Error occurred while saving the data');
        }
    });
}
