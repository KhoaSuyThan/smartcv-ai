const container = document.querySelector('.container');
const registerBtn = document.querySelector('.register-btn');
const loginBtn = document.querySelector('.login-btn');

registerBtn.addEventListener('click', () => {
    container.classList.add('active');
})

loginBtn.addEventListener('click', () => {
    container.classList.remove('active');
})

document.addEventListener('DOMContentLoaded', function () {
    const roleSelect = document.getElementById('roleSelect');
    const companyBox = document.getElementById('companyBox');

    if (roleSelect && companyBox) {
        roleSelect.addEventListener('change', function () {
            if (this.value === 'Recruiter') {
                companyBox.style.display = 'block';
            } else {
                companyBox.style.display = 'none';
            }
        });
    }
});