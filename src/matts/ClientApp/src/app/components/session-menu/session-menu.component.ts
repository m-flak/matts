import { Component, EventEmitter, Input, OnInit, Output } from '@angular/core';

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

  @Output()
  clickLogout: EventEmitter<void> = new EventEmitter();

  constructor() { }

  ngOnInit(): void {
  }

}
