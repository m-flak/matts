import { Component, OnDestroy, OnInit } from '@angular/core';
import { LoadingBarService } from '@ngx-loading-bar/core';
import { Subscription } from 'rxjs';

@Component({
  selector: 'app-root-home',
  templateUrl: './app-root-home.component.html'
})
export class AppRootHomeComponent implements OnInit, OnDestroy {
  title = 'app';

  loader = this.loadingBar.useRef();
  subscription!: Subscription | null;
  displayDimmer = false;

  constructor(private loadingBar: LoadingBarService) {}
  
  ngOnInit(): void {
    this.subscription = this.loader.value$.subscribe({
      next: _ => {
        this.displayDimmer = true;
      },
      complete: () => {
        this.displayDimmer = false;
      }
    })
  }

  ngOnDestroy(): void {
    if (this.subscription !== null) {
      this.subscription.unsubscribe();
      this.subscription = null;
    }
  }
}
