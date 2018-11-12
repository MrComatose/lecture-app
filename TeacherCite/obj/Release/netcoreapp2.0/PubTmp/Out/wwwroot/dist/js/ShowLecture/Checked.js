const checkedVisit = (lectureId, userName) => {
	var jqxhr = $.post("/Admin/Students/ChekVisit",
		{ UserName: userName, LectureID: lectureId }
		, function () {
			M.toast({ html: '<blockquote>Student: ' + userName + '  checked!</blockquote>', classes: 'rounded' });
			setTimeout(()=>M.Toast.dismissAll(),1000);
			
	});


};