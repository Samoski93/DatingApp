import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { map } from 'rxjs/operators';
import { JwtHelperService } from '@auth0/angular-jwt';

@Injectable()
export class AuthService {
  baseUrl = 'http://localhost:5000/api/auth/';
  jwtHelper = new JwtHelperService();
  decodedToken: any;

  constructor(private http: HttpClient) {}

  // Takes the model object passed from the navbar
  login(model: any) {
    return (
      this.http
        .post(this.baseUrl + 'login', +model)
        // Server will return a token, take response from the server and transform it
        .pipe(
          map((response: any) => {
            const user = response; // This will hold the token object
            if (user) {
              localStorage.setItem('token', user.token); // Stores the the token locally, so we have easy access to it
              this.decodedToken = this.jwtHelper.decodeToken(user.token);
              console.log(this.decodedToken);
            }
          })
        )
    );
  }

  register(model: any) {
    return this.http.post(this.baseUrl + 'register', model);
  }

  loggedIn() {
    // Get token from localStorage
    const token = localStorage.getItem('token');
    return !this.jwtHelper.isTokenExpired(token); // bool - if token is not expired
  }
}
