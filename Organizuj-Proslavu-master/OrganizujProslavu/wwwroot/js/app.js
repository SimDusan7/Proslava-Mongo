const dugme=document.querySelector(".dugme");
const navlinks=document.querySelector(".nav-links");

dugme.addEventListener('click',()=>{
    navlinks.classList.toggle("nav-active");
    
});