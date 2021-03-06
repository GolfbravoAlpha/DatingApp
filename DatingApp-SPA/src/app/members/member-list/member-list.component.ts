import { Pagination, PaginatedResult } from './../../_models/pagination';
import { ActivatedRoute } from '@angular/router';
import { AlertifyService } from '../../_services/alertify.service';
import { UserService } from '../../_services/user.service';
import { Component, OnInit } from '@angular/core';
import { User } from '../../_models/user';

@Component({
  selector: 'app-member-list',
  templateUrl: './member-list.component.html',
  styleUrls: ['./member-list.component.css']
})
export class MemberListComponent implements OnInit {
  users: User[];
  user: User = JSON.parse(localStorage.getItem('user'));
  genderList = [{value: 'male', display: 'Males'},
               {value: 'female', display: 'Females'}];
  userParams: any = {};
  pagination: Pagination;

  constructor(
    private userService: UserService,
    private alertify: AlertifyService,
    private route: ActivatedRoute
    ) { }

  ngOnInit() {
    this.route.data.subscribe(data => {
      this.users = data['users'].result;
      // give pagination of users to the template
      this.pagination = data['users'].pagination;
    });

    this.userParams.gender = this.user.gender === 'female' ? 'male' : 'female';
    this.userParams.minAge = 18;
    this.userParams.maxAge = 99;
    this.userParams.orderBy = 'lastActive';
  }


  resetFilters() {
    this.userParams.gender = this.user.gender === 'female' ? 'male' : 'female';
    this.userParams.minAge = 18;
    this.userParams.maxAge = 99;
    this.loadUsers();
  }

  pageChanged(event: any): void {
    this.pagination.currentPage = event.page;
    // return the new users for the new batch of users
    this.loadUsers();
  }

  // new batch of users method
  loadUsers() {
    // use the api method of 'http://localhost:5000/api/users?PageNumber=3&pageSize=3'
    // using the 'userService' method of getUsers, inputing current page number and the size
    this.userService.getUsers(this.pagination.currentPage, this.pagination.itemsPerPage, this.userParams)
      .subscribe((res: PaginatedResult<User[]>) => {
      this.users = res.result;
      this.pagination = res.pagination;
    }, error => {
      this.alertify.error(error);
    });
  }
}
