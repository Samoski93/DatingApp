import { environment } from '../../environments/environment';
import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { User } from '../_models/user';
import { JwtHelperService } from '@auth0/angular-jwt';
import { map } from 'rxjs/operators';
import { PaginatedResult } from '../_models/pagination';

// Get users bearer token
// const httpOptions = {
//   headers: new HttpHeaders({
//     Authorize: 'Bearer ' + localStorage.getItem('token')
//   })
// };

@Injectable({
  providedIn: 'root'
})
export class UserService {
  baseUrl = environment.apiUrl + 'auth/';
  jwtHelper = new JwtHelperService();
  decodedToken: any;
  currentUser: User;

  constructor(private http: HttpClient) {}

  login(model: any) {
    return this.http
      .post(this.baseUrl + 'login', model, this.requestOptions())
      .pipe(
        map((response: any) => {
          const user = response;
          if (user) {
            localStorage.setItem('token', user.token);
            localStorage.setItem('user', JSON.stringify(user.user));
            this.decodedToken = this.jwtHelper.decodeToken(user.token);
            this.currentUser = user.user;
            console.log(this.decodedToken);
          }
        })
      );
  }

  // register(model: any) {
  //   return this.http.post(this.baseUrl + 'register', model);
  // }

  // returning a user array - get() returns an object of type user
  getUsers(
    page?,
    itemsPerPage?,
    userParams?
  ): Observable<PaginatedResult<User[]>> {
    const paginatedResult: PaginatedResult<User[]> = new PaginatedResult<
      User[]
    >();

    let params = new HttpParams();

    if (page != null && itemsPerPage != null) {
      params = params.append('pageNumber', page);
      params = params.append('pageSize', itemsPerPage);
    }

    if (userParams != null) {
      params = params.append('minAge', userParams.minAge);
      params = params.append('maxAge', userParams.maxAge);
      params = params.append('gender', userParams.gender);
      params = params.append('orderBy', userParams.orderBy);
    }

    return this.http
      .get<User[]>(this.baseUrl + 'users', { observe: 'response', params })
      .pipe(
        // pipe allows access to rxjs operators
        map(response => {
          paginatedResult.result = response.body;
          if (response.headers.get('Pagination') != null) {
            paginatedResult.paginatioon = JSON.parse(
              response.headers.get('Pagination')
            );
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

  setMainPhoto(userId: number, id: number) {
    return this.http.post(
      this.baseUrl + 'users/' + userId + '/photos/' + id + '/setMain',
      {}
    );
  }

  deletePhoto(userId: number, id: number) {
    return this.http.delete(this.baseUrl + 'users/' + userId + '/photos/' + id);
  }
}
