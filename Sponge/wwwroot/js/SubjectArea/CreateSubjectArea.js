//To validate the fields on client side
//function validateForm(form) {
//    var isValid = true;

//    $(':input[required]:visible', form).each(function () {
//        if ($(this).val().trim() === '') {
//            isValid = false;
//            $(this).css('border', '1px solid red');
//        } else {
//            $(this).css('border', '');
//        }
//    });

//    if (!isValid) {
//        alert('Please fill all required fields.');
//    }
//    return isValid;
//}
$(document).ready(function () {
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
                    debugger;
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
