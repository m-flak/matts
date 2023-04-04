import { Component, Input, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { Job } from '../models';

export type JobPageRouteData = {currentJob: Job };

@Component({
  selector: 'app-job-page',
  templateUrl: './job-page.component.html',
  styleUrls: ['./job-page.component.scss']
})
export class JobPageComponent implements OnInit {
  @Input()
  currentJob: Job | null = null;

  constructor(private router: Router) { }

  ngOnInit(): void {
    const state: JobPageRouteData = this.router.getCurrentNavigation()?.extras.state as JobPageRouteData;
    console.error(state);
    this.currentJob = state?.currentJob ?? null;
  }

}
