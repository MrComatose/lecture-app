---
description: >-
  В поточній версії пояснюється яким чином влаштоване обрізання фотографії на
  стороні клієнта.
---

# Версія 3. Механізм обрізання зображень

Для зменшення навантаження на пропускну здатність мережі та як наслідок покращення продуктивності сайту використовується обрізання усіх зображень які потрапляють у бд. 

## Завантаження аватару студента.

Html:

Input:

```markup
  <div class="input-field col m4">
                        @if (Model.Avatar == null)
                        {
                            <img id="blah" class="circle" style="object-fit: cover;width:200px;height:200px;" src="~/img/img_avatar.png">
                        }
                        else
                        {
                            <img id="blah" class="circle" style="object-fit: cover;width:200px;height:200px;" src="data:image;base64,@(Convert.ToBase64String(Model.Avatar))">
                        }
                        <div class="file-field input-field">
                            <div class="btn">
                                <span><i class="material-icons">get_app</i></span>
                                <input type="file" accept="image/x-png,image/gif,image/jpeg" onchange="readURL(this);" />
                            </div>

                            <div class="file-path-wrapper">
                                <input class="file-path validate" type="text" placeholder="Upload avatar:" />
                            </div>
                            <span class="red-text"></span>

                        </div>
                    </div>

```

При завантаженні картинки викликається функція readURL\(this\), яка має наступний вигляд:

```javascript
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
```

В функції виконується перевірка розширення файлу після чого, якщо файл це картинка виконується налаштування об'єкту Cropper до якого,а саме при завантаженні файлу створюється об'єкт Cropper та відкривається модальне вікно на якому розташований канвас для обрізання. При зміні рамок обрізання генерується новий канвас 200 на 200 пікселів з якого дані записуються у out у вигляді URI об'єкту.

Для відправлення зображення на сервер використовується наступна функція:

```javascript
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
```

Для конвертації URI у масив байтів використовується наступна функція.

```javascript
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

```

## Підсумок 

Таким чином відбувається завантаження зображень для студентів, аналогічно реалізовано і для аккаунту вчителя, і для завантаження зображень новин. Реалізацію можна знайти на GitHub. Попри покращення продуктивності данний функціонал також гарантує гарне відображення зображень на сайті, так як вони обрізаються під потрібний розмір.

