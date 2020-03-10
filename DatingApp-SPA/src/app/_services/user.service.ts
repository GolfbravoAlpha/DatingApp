import { map } from 'rxjs/operators';
import { PaginatedResult } from './../_models/pagination';
import { environment } from './../../environments/environment';
import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders, HttpParams } from '@angular/common/http';
import { User } from '../_models/user';
import { Observable } from 'rxjs';




@Injectable({
  providedIn: 'root'
})
export class UserService {
  baseUrl = environment.apiUrl;

  constructor(private http: HttpClient) { }

  // this method is returning paginatedResult of type User[]
  getUsers(page?, itemsPerPage?, userParams?, likesParam?): Observable<PaginatedResult<User[]>> {
    // user object to set as the padinatedResult for the generic class.
    // we have to create a new instance of this class with 'new PaginatedResult<User[]>()'
    const paginatedResult: PaginatedResult<User[]> = new PaginatedResult<User[]>();

    let params = new HttpParams();

    // checking if pageNumber and pageSize has been entered
    if (page != null && itemsPerPage != null) {
      // what to put in http://localhost:5000/api/users?PageNumber=3&pageSize=3
      // remember append means to put on the end of, therefore, the below goes at the end of the API url as shown above
      params = params.append('pageNumber', page);
      params = params.append('pageSize', itemsPerPage);
    }

    // if user object has been passed such as minAge, maxAge or orderBy, add the information to the httpParams()
    if (userParams != null) {
      params = params.append('minAge', userParams.minAge);
      params = params.append('maxAge', userParams.maxAge);
      params = params.append('gender', userParams.gender);
      params = params.append('orderBy', userParams.orderBy);
    }

    // get users that you have liked
    if (likesParam === 'Likers') {
      params = params.append('likers', 'true');
    }

    // get users that have liked you
    if (likesParam === 'Likees') {
      params = params.append('likees', 'true');
    }

    // using api parameter e.g "http://localhost:5000/api/users?PageNumber=3&pageSize=3"
    return this.http.get<User[]>(this.baseUrl + 'users', { observe: 'response', params})
      .pipe( // we use pipe becuase were getting pagination data as well as user + message data
        map (response => {
          paginatedResult.result = response.body;
          if (response.headers.get('Pagination') != null) {
            // convert from string back to object of the pagination
            paginatedResult.pagination = JSON.parse(response.headers.get('Pagination'));
          }
          return paginatedResult;
        })
      );
  }


  getUser(id): Observable<User> {
    return this.http.get<User>(this.baseUrl + 'users/' + id);
  }

  updateUser(id: number, user: User) {
    return this.http.put(this.baseUrl + 'users/' + id, user);
  }

  setMainPhoto(UserId: number, id: number) {
    return this.http.post(this.baseUrl + 'users/' + UserId + '/photos/' + id + '/setMain', {});
  }

  deletePhoto(userId: number, id: number) {
    return this.http.delete(this.baseUrl + 'users/' + userId + '/photos/' + id);
  }

  sendLike(id: number, recipientId: number) {
    return this.http.post(this.baseUrl + 'users/' + id + '/like/' + recipientId, {})
  }
}
