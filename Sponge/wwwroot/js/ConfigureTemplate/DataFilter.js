const dimensionCheckBoxes = document.querySelectorAll('.dropdown-menu input[name="dimensionSelect"]');
const dpBtn = document.getElementById('dimensionSelectID');
//const dpRole = document.getElementById('Role');
let mySelectedListItems = [];
let mySelectedListItemsRole = [];

function handleCB() {
    mySelectedListItems = [];
    let mySelectedListItemsText = '';

    dimensionCheckBoxes.forEach((checkbox) => {
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
//function handleRoleCB() {
//    mySelectedListItemsRole = [];
//    let mySelectedListItemsTextRole = '';
//    chRoleBoxes.forEach((checkbox) => {
//        if (checkbox.checked) {
//            let checkboxText = $(`label[for="${checkbox.value}"]`).text();
//            mySelectedListItemsRole.push(checkbox.value);
//            mySelectedListItemsTextRole += checkboxText.trim() + ', ';
//        }
//    });

//    dpRole.innerText =
//        mySelectedListItemsRole.length > 0
//            ? mySelectedListItemsTextRole.slice(0, -2) : 'Select';
//}

dimensionCheckBoxes.forEach((checkbox) => {
    checkbox.addEventListener('change', handleCB);
});
//chRoleBoxes.forEach((checkbox) => {
//    checkbox.addEventListener('change', handleRoleCB);
//});

$(document).ready(function () {

    handleCB();

    $("#getSubDimensionsBTN").click(function () {

       // let values = $('#dimensionSelectID').val();
        let values = document.getElementById("dimensionSelectID").textContent;
        let dimensionsArray = values.split(", ");
        if (dimensionsArray == "Select") {
            alert('Please select at least one dimension');
            document.getElementById('submitSelectionsID').style.visibility = 'hidden';
        }
        else {
            $.ajax({
                type: 'post',
                url: '/ConfigureTemplate/GetSubDimensionsList',
                data: {
                    'dimensions': dimensionsArray
                },
                success: function (data) {
                    //Empty the div everytime after the ajax call
                    $('#dropdownsContainer').empty();
                    // Iterate over each dimension in data
                    $.each(data, function (dimension, names) {
                        var html = '<div class="col-md-4"><div class="wrapper">';
                        html += '<label>' + dimension + '</label><button type="button" style="border-radius:5px" class="form-control toggle-next ellipsis">--Select--</button>';
                        html += '<div class="checkboxes" ><div class="inner-wrap">';

                        $.each(names, function (i, name) {
                            html += '<label><input type="checkbox" value="' + name + '" class="ckkBox val" /><span>' + name + '</span></label><br>';
                        });

                        html += '</div></div></div></div>';

                        $('#dropdownsContainer').append(html);

                        // Add some space after each three dropdowns
                        if ((dimension + 1) % 3 == 0) {
                            $('#dropdownsContainer').append('<div style="clear:both;height:20px;"></div>');
                        }
                    });
                    document.getElementById('submitSelectionsID').style.visibility = 'visible';
                },
                error: function (error) {

                    console.log(error);

                }
            });
        }
        
    });

});
$(document).on('click', '.toggle-next', function () {
    $(this).next('.checkboxes').slideToggle(50);
});

$(document).on('change', '.ckkBox', function () {
    toggleCheckedAll(this);
    setCheckboxSelectLabels();
});

//Multiselect dropdown
$(function () {
    setCheckboxSelectLabels();

    // Select All
    //$('.select_all_below').on('change', function () {
    //    $(this).closest('.inner-wrap').find('.ckkBox').prop('checked', $(this).prop('checked'));
    //});

    //$('.inner-wrap .ckkBox').on('change', function () {
    //    var isAllChecked = $(this).closest('.inner-wrap').find('.ckkBox:not(:checked)').length === 0;
    //    $(this).closest('.inner-wrap').find('.select_all_below').prop('checked', isAllChecked);
    //});

    //$('.toggle-next').click(function () {
    //    $(this).next('.checkboxes').slideToggle(50);
    //});


    //$('.ckkBox').change(function () {
    //    toggleCheckedAll(this);
    //    setCheckboxSelectLabels();
    //});

});

$(document).on('click', function (event) {
    if (!$(event.target).closest('.toggle-next, .checkboxes').length) {
        $('.checkboxes').slideUp(50);
    }
});

function setCheckboxSelectLabels(elem) {
    var wrappers = $('.wrapper');
    
    $.each(wrappers, function (key, wrapper) {
        var checkboxes = $(wrapper).find('.ckkBox');
        var label = $(wrapper).find('.checkboxes').attr('id');
        var prevText = '';
        var completeText = ''; 
        var button = $(wrapper).find('button');
        $.each(checkboxes, function (i, checkbox) {
            
            if ($(checkbox).prop('checked') == true) {
                var text = $(checkbox).next().html();
                var btnText = prevText + text;
                var numberOfChecked = $(wrapper).find('input.val:checkbox:checked').length;
                completeText += text + ', ';
                if (numberOfChecked >= 4) {
                    btnText = numberOfChecked + ' selected';
                }
                $(button).text(btnText);
                prevText = btnText + ', ';
                
            }
            
        });
        // Generate a unique name for the hidden input.
        var hiddenInputName = 'completeText' + key;

        // Check if an input with this name already exists.
        var hiddenInput = $("input[name='" + hiddenInputName + "']");

        // If an input with this name does not exist, create it.
        if (hiddenInput.length == 0) {
            hiddenInput = $('<input/>', {
                type: 'hidden',
                name: hiddenInputName,
                value: completeText
            });
            $('#selectionContainerID').append(hiddenInput);
        } else {
            // If an input with this name exists, update its value.
            hiddenInput.val(completeText);
        }

        // Check if any checkbox is checked after the loop.
        var totalChecked = $(checkboxes).filter(':checked').length;

        if (totalChecked == 0) {
            $(button).text('--Select--');
        }
        console.log(completeText);
    });
    
}

function toggleCheckedAll(checkbox) {
    var apply = $(checkbox).closest('.wrapper').find('.apply-selection');
    apply.fadeIn('slow');

    var val = $(checkbox).closest('.checkboxes').find('.val');
    var all = $(checkbox).closest('.checkboxes').find('.all');
    var ckkBox = $(checkbox).closest('.checkboxes').find('.ckkBox');

    if (!$(ckkBox).is(':checked')) {
        $(all).prop('checked', true);
        return;
    }

    if ($(checkbox).hasClass('all')) {
        $(val).prop('checked', false);
    } else {
        $(all).prop('checked', false);
    }
}