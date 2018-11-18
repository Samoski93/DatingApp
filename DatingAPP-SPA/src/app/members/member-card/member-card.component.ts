import { Component, OnInit, Input } from '@angular/core';
import { User } from 'src/app/_models/user';

@Component({
  selector: 'app-member-card',
  templateUrl: './member-card.component.html',
  styleUrls: ['./member-card.component.css']
})
export class MemberCardComponent implements OnInit {
  // Pass down user from parent component(member-list) down to this child component
  @Input()
  user: User;
  constructor() {}

  ngOnInit() {}
}
