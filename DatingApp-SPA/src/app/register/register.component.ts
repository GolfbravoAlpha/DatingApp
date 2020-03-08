import { Router } from '@angular/router';
import { AlertifyService } from './../_services/alertify.service';
import { AuthService } from '../_services/auth.service';
import { Component, OnInit, Input, Output, EventEmitter } from '@angular/core';
import { FormGroup, FormControl, Validators, FormBuilder } from '@angular/forms';
import { BsDatepickerConfig } from 'ngx-bootstrap';
import { User } from '../_models/user';


@Component({
  selector: 'app-register',
  templateUrl: './register.component.html',
  styleUrls: ['./register.component.css']
})
export class RegisterComponent implements OnInit {
  @Output() cancelRegister = new EventEmitter();
  user: User;
  registerForm: FormGroup; // for reactive forms
  bsConfig: Partial<BsDatepickerConfig>; // datepicker from ngx, to change color

  constructor(
    private authService: AuthService,
    private router: Router,
    private alertify: AlertifyService,
    private fb: FormBuilder) { }

  ngOnInit() {
    this.bsConfig = {
      containerClass: 'theme-red'
    };
    this.createRegisterForm();
  }
  // Reactive forms
  createRegisterForm() {
    this.registerForm = this.fb.group({
      gender: ['male'],
      username: ['', Validators.required],
      knownAs: ['', Validators.required],
      dateOfBirth: [null, Validators.required],
      city: ['', Validators.required],
      country: ['', Validators.required],
      password: ['', [Validators.required, Validators.minLength(4), Validators.maxLength(8)]],
      confirmPassword: ['', Validators.required]
    }, {validator: this.passwordMatchValidator});
  }

  // check if password and confirmPassword match
  passwordMatchValidator(g: FormGroup) {
    return g.get('password').value === g.get('confirmPassword').value ? null : {mismatch : true};
  }

  // if register button is clicked
  register() {
    if (this.registerForm.valid) {
      // copy values of the form such as name, knownAs, DOB into the empty object of "{}"
      this.user = Object.assign({}, this.registerForm.value);
      // go to the api and register the user
      this.authService.register(this.user).subscribe(() => {
        this.alertify.success('Registration successful');
      }, error => {
        this.alertify.error(error);
      }, () => {
        // log the user in after they have successfully registered 
          this.authService.login(this.user).subscribe(() => {
            this.router.navigate(['/members']);
            console.log('routing worked');
        });
      });
    }
  }

  cancel() {
    this.cancelRegister.emit(false);
  }
}
