let mesbutton = document.getElementById('bot');
let otinnhan = document.getElementsByClassName('otinnhan');
let closebutton = document.getElementById('close');
closebutton.addEventListener('click', function () {
    otinnhan[0].style.display = 'none';
});
mesbutton.addEventListener('click', function () {
    otinnhan[0].style.display = 'block';
});
r