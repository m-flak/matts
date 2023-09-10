import { Component } from '@angular/core';
import { LoadingBarService } from '@ngx-loading-bar/core';

@Component({
  selector: 'app-root-home',
  templateUrl: './app-root-home.component.html'
})
export class AppRootHomeComponent {
  title = 'app';

  loader = this.loadingBar.useRef();
  constructor(private loadingBar: LoadingBarService) {}
}
