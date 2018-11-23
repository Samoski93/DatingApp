import { Component, OnInit, Input } from '@angular/core';
import { User } from 'src/app/_models/user';
import { AuthService } from 'src/app/_services/auth.service';
import { AlertifyService } from 'src/app/_services/alertify.service';
import { UserService } from 'src/app/_services/user.service';
import { ActivatedRoute } from '@angular/router';

@Component({
  selector: 'app-member-card',
  templateUrl: './member-card.component.html',
  styleUrls: ['./member-card.component.css']
})
export class MemberCardComponent implements OnInit {
  // Pass down user from parent component(member-list) down to this child component
  @Input()
  user: User;

  constructor(
    private authService: AuthService,
    private alertify: AlertifyService,
    private userService: UserService
  ) {}

  ngOnInit() {}

  // Send a like,id is the recipientId which we will get when the user clicks on the like button
  sendLike(id: number) {
    this.userService
      .sendLike(this.authService.decodedToken.nameId, id)
      .subscribe(
        data => {
          this.alertify.success('You have liked: ' + this.user.knownAs);
        },
        error => {
          this.alertify.error(error);
        }
      );
  }
}
