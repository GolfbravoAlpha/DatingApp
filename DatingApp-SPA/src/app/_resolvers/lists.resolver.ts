import { catchError } from 'rxjs/operators';
import { Observable, of } from 'rxjs';
import { AlertifyService } from '../_services/alertify.service';
import { UserService } from '../_services/user.service';
import { Injectable } from '@angular/core';
import { User } from '../_models/user';
import { Resolve, Router, ActivatedRouteSnapshot } from '@angular/router';


@Injectable()
export class ListsResolver implements Resolve<User[]> {
    // send pagination argument into the parameter of the api
    pageNumber = 1;
    pageSize = 5;
    likesParam = 'Likers';

    constructor(
        private userService: UserService,
        private router: Router,
        private alertify: AlertifyService) {}

    resolve(route: ActivatedRouteSnapshot): Observable<User[]> {
        return this.userService.getUsers(this.pageNumber, this.pageSize, null, this.likesParam).pipe(
            catchError(error => {
                this.alertify.error('Problems retreving data');
                this.router.navigate(['/home']);
                return of(null);
            })
        );
    }
}
