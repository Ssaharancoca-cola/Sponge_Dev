// Call this function to trigger the update
function updateSubjectArea() {
    $.ajax({
        url: '/SubjectArea/UpdateSubjectArea', // Replace [Controller] with your controller name
        type: 'POST',
        data: $('#editSubjectAreaFormID').serialize(),
        success: function (response) {
            if (response.success) {
                alert(response.message);
                window.location.href = '/SubjectArea/ManageSubjectArea';
            } else {
                alert(response.message);
            }
        },
        error: function () {
            alert('Error occurred while updating the subject area');
        }
    });
}