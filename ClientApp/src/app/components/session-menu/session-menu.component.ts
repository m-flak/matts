import { Component, Input, OnInit } from '@angular/core';

@Component({
  selector: 'cmp-session-menu',
  templateUrl: './session-menu.component.html',
  styleUrls: ['./session-menu.component.scss']
})
export class SessionMenuComponent implements OnInit {
  @Input()
  userFullName = '';

  @Input()
  userProfilePic = '';

  constructor() { }

  ngOnInit(): void {
  }

}
