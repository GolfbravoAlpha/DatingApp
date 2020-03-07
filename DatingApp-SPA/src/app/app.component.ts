import { User } from './_models/user';
import { AuthService } from './_services/auth.service';
import { Component, OnInit } from '@angular/core';
import {JwtHelperService} from '@auth0/angular-jwt';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.css']
})
export class AppComponent implements OnInit {
  jwtHelper = new JwtHelperService();


  constructor(private authService: AuthService) {}

  ngOnInit() {
    const token = localStorage.getItem('token');
    // json.parse converts the string back into an object from the browser
    const user: User = JSON.parse(localStorage.getItem('user'));
    if (token) {
      this.authService.decodedToken = this.jwtHelper.decodeToken(token);
    }
    if (user) {
      this.authService.currentUser = user;
      // update current photo in authService with the user object's photo in browser storage
      this.authService.changeMemberPhoto(user.photoUrl);
    }
  }
}
