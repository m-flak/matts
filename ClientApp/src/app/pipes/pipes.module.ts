import { NgModule } from "@angular/core";
import { ResumeNamePipe } from "./resume-name.pipe";

@NgModule({
    declarations: [
        ResumeNamePipe
    ],
    exports: [
        ResumeNamePipe
    ]
})
export class PipesModule {

}
