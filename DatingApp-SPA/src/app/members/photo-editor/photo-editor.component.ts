import { AlertifyService } from './../../_services/alertify.service';
import { UserService } from './../../_services/user.service';
import { AuthService } from './../../_services/auth.service';
import { environment } from './../../../environments/environment';
import { Component, OnInit, Input, Output, EventEmitter } from '@angular/core';
import { FileUploader } from 'ng2-file-upload';
import { Photo } from 'src/app/_models/photo';

@Component({
  selector: 'app-photo-editor',
  templateUrl: './photo-editor.component.html',
  styleUrls: ['./photo-editor.component.css']
})
export class PhotoEditorComponent implements OnInit {
  @Input() photos: Photo[];
  @Output() getMemberPhotoChange = new EventEmitter<string>();
  uploader: FileUploader;
  hasBaseDropZoneOver = false;
  baseUrl = environment.apiUrl;
  currentMain: Photo;


  constructor(
    private authService: AuthService,
    private userService: UserService,
    private alertify: AlertifyService
    ) { }

  ngOnInit() {
    this.initializeUploader();
  }

  fileOverBase(e: any): void {
    this.hasBaseDropZoneOver = e;
  }

  initializeUploader() {
    this.uploader = new FileUploader({
      url: this.baseUrl + 'users/' + this.authService.decodedToken.nameid + '/photos',
      authToken: 'Bearer ' + localStorage.getItem('token'),
      isHTML5: true,
      allowedFileType: ['image'],
      removeAfterUpload: true,
      autoUpload: false,
      maxFileSize: 10 * 1024 * 1024
    });

    this.uploader.onAfterAddingFile = (file) => {file.withCredentials = false; };

    this.uploader.onSuccessItem = (item, response, status, headers) => {
      if (response) {
        const res: Photo = JSON.parse(response);
        const photo = {
          id: res.id,
          url: res.url,
          dateAdded: res.dateAdded,
          description: res.description,
          isMain: res.isMain
        };
        this.photos.push(photo);
        // update first photo sent to navbar and memberedit
        if (photo.isMain) {
          this.authService.changeMemberPhoto(photo.url);
          // Allows main photo to stay the same everywhere even after refreshing the web page.
          this.authService.currentUser.photoUrl = photo.url;
          localStorage.setItem('user', JSON.stringify(this.authService.currentUser));
        }
      }
    };
  }

  setMainPhoto(photo: Photo) {
    this.userService.setMainPhoto(this.authService.decodedToken.nameid, photo.id).subscribe(() => { // set main photo to the backend
      this.currentMain = this.photos.filter(p => p.isMain === true)[0]; // access the current main photo
      this.currentMain.isMain = false; // set current main photo to false
      photo.isMain = true; // set photo object that has been sent into this method as the main photo
      // send the updated photo to authservice changememberphoto() method
      // it will then update member-edit and navbar component photos.
      this.authService.changeMemberPhoto(photo.url);
      // Allows main photo to stay the same everywhere even after refreshing the web page.
      this.authService.currentUser.photoUrl = photo.url;
      localStorage.setItem('user', JSON.stringify(this.authService.currentUser));
      // the emit will also trigger the updateMainPhoto() method in member-edit
      // which will update the current photo to the one that has isMain to true
    }, error => {
      this.alertify.error(error);
    });
  }


  // delete method to delete photo
  deletePhoto(id: number) {
    // ask user if they are sure
    this.alertify.confirm('Are you sure you want to delete this photo?', () => {
      // run deletePhoto method from userService.
      this.userService.deletePhoto(this.authService.decodedToken.nameid, id).subscribe(() => {
        // remove photo from array thats stored locally.
        this.photos.splice(this.photos.findIndex(p => p.id === id), 1);
        this.alertify.success('photo has been deleted');
      }, error => {
        this.alertify.error('Failed to delete the photo');
      });
    });
  }
}
