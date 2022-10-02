const navBtn = document.querySelector('.header__mobile-nav');
const mobileNav = document.querySelector('.mobile-nav');
const header = document.querySelector('.header');
const main = document.querySelector('.main-block');
const join = document.querySelector('.join');
const body = document.body;
navBtn.addEventListener('click',function(event){
    event.stopPropagation();
    mobileNav.classList.toggle('mobile-nav--active');
    body.classList.toggle('no-scroll');
    header.classList.toggle('blackout');
    main.classList.toggle('blackout');
    join.classList.toggle('blackout');
    
});
mobileNav.addEventListener('click',function(event){
    event.stopPropagation();
})
window.addEventListener('click',function(){
    if (body.classList.contains('no-scroll')){
        body.classList.toggle('no-scroll');
        mobileNav.classList.toggle('mobile-nav--active');
        header.classList.toggle('blackout');
        main.classList.toggle('blackout');
        join.classList.toggle('blackout');
    }
});