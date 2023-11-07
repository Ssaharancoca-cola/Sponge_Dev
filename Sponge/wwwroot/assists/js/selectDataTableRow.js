const selectAll = document.getElementById('selectAll');
const selectRowCheckboxes = document.querySelectorAll('.select-row');

selectAll.addEventListener('change', function () {
    for (const checkbox of selectRowCheckboxes) {
        checkbox.checked = selectAll.checked;
    }
});

for (const checkbox of selectRowCheckboxes) {
    checkbox.addEventListener('change', function () {
        const allChecked = Array.from(selectRowCheckboxes).every((checkbox) => checkbox.checked);
        selectAll.checked = allChecked;
    });
}
