var _validFileExtensions = [".jpg", ".jpeg", ".bmp", ".gif", ".png"];
var cropper;
var out;
var username = document.getElementById('OldUserName').value;
var outimage = document.getElementById('blah');
$(document).ready(function () {
	$('#crop').modal();
});
function readURL(input) {
	if (input.files && input.files[0]) {
		var reader = new FileReader();
		if (cropper) {
			cropper.destroy();
		}
		var sFileName = input.value;
		var validate = false;
		for (var j = 0; j < _validFileExtensions.length; j++) {
			var sCurExtension = _validFileExtensions[j];
			if (sFileName.substr(sFileName.length - sCurExtension.length, sCurExtension.length).toLowerCase() === sCurExtension.toLowerCase()) {
				validate = true;

				break;
			}
		}


		if (validate) {
			reader.onload = function (e) {

				$('#image')
					.attr('src', e.target.result);
				var image = document.getElementById('image');



				image.crossOrigin = "anonymous";

				cropper = new Cropper(image, {
					aspectRatio: 1 / 1,
					crop(event) {

						let canvas = this.cropper.getCroppedCanvas({
							width: 200,
							height: 200,

							maxWidth: 800,
							maxHeight: 800,
							fillColor: '#fff',

						});

						out = canvas.toDataURL();

					},
				});


			};
			reader.readAsDataURL(input.files[0]);
			$('#crop').modal('open');
		} else {

			input.value = null;

		}


	}
}

const sendImage = () => {
	if (out) {
		var formData = new FormData();
		formData.set("file", dataURItoBlob(out));
		formData.set("username", username);

		$.ajax('/Student/Account/Avatar', {
			method: "POST",
			data: formData,
			processData: false,
			contentType: false,
			success() {
				outimage.src = out;
			},
			error() {

				alert('Upload error');
			},
		});
	}

}
function dataURItoBlob(dataURI) {
	// convert base64 to raw binary data held in a string
	// doesn't handle URLEncoded DataURIs - see SO answer #6850276 for code that does this
	var byteString = atob(dataURI.split(',')[1]);

	// separate out the mime component
	var mimeString = dataURI.split(',')[0].split(':')[1].split(';')[0];

	// write the bytes of the string to an ArrayBuffer
	var ab = new ArrayBuffer(byteString.length);
	var ia = new Uint8Array(ab);
	for (var i = 0; i < byteString.length; i++) {
		ia[i] = byteString.charCodeAt(i);
	}

	//Old Code
	//write the ArrayBuffer to a blob, and you're done
	//var bb = new BlobBuilder();
	//bb.append(ab);
	//return bb.getBlob(mimeString);

	//New Code
	return new Blob([ab], { type: mimeString });


}
