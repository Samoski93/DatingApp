import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BehaviorSubject } from 'rxjs';
import { map } from 'rxjs/operators';
import { JwtHelperService } from '@auth0/angular-jwt';
import { environment } from 'src/environments/environment';
import { User } from '../_models/user';

// Auth service to query database
@Injectable()
export class AuthService {
  baseUrl = environment.apiUrl + 'auth/';
  jwtHelper = new JwtHelperService();
  decodedToken: any;
  currentUser: User;
  // memberEdit & navComp subscribes from this
  photoUrl = new BehaviorSubject<string>('../../assets/user.png'); // photoUrl is now an observable of type BehaviorSubject,
  currentPhotoUrl = this.photoUrl.asObservable();

  constructor(private http: HttpClient) {}

  // This method is called when a user updates is photo (main)
  changeMemberPhoto(photoUrl: string) {
    this.photoUrl.next(photoUrl);
  }

  // Takes the model object passed from the navbar
  // login(model: any) {
  //   return (
  //     this.http
  //       .post(this.baseUrl + 'login', model, this.requestOptions())
  //       // Server will return a token, take response from the server and transform it
  //       .pipe(
  //         map((response: any) => {
  //           const user = response; // This will hold the token object
  //           if (user) {
  //             localStorage.setItem('token', user.token); // Stores the the token locally, so we have easy access to it
  //             localStorage.setItem('user', JSON.stringify(user.user));
  //             this.decodedToken = this.jwtHelper.decodeToken(user.token);
  //             this.changeMemberPhoto(this.currentUser.photoUrl);
  //           }
  //         })
  //       )
  //   );
  // }

  login(model: any) {
    return this.http.post(this.baseUrl + 'login', model).pipe(
      map((response: any) => {
        const user = response;
        if (user) {
          localStorage.setItem('token', user.token);
          localStorage.setItem('user', JSON.stringify(user.user));
          this.decodedToken = this.jwtHelper.decodeToken(user.token);
          this.currentUser = user.user;
          this.changeMemberPhoto(this.currentUser.photoUrl);
        }
      })
    );
  }

  register(user: User) {
    return this.http.post(this.baseUrl + 'register', user);
  }

  loggedIn() {
    // Get token from localStorage
    const token = localStorage.getItem('token');
    return !this.jwtHelper.isTokenExpired(token); // bool - if token is not expired
  }
}
