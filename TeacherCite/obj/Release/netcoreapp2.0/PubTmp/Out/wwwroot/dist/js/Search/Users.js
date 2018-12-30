const getUsers = (sample) => {
   let item =document.getElementById('list');
   item.innerHTML="<div class='progress'><div class='indeterminate' style='width: 70%'></div></div>";
    
     var formData = new FormData();
     formData.set("str", sample);
     
 
     $.ajax("/Teacher/Search/Users", {
       method: "POST",
       data: formData,
       processData: false,
       contentType: false,
       success(data) {
        
         item.innerHTML=data;
       
       },
       error(){
         item.innerHTML="<h1 class='red-text'>server-rror</h1>";
       }
       
     });
   
 };