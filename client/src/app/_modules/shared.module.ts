import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { BsDropdownModule } from 'ngx-bootstrap/dropdown';
import { ToastrModule } from 'ngx-toastr';
import { TabsModule } from 'ngx-bootstrap/tabs';
import { MemberDetailComponent } from '../members/member-detail/member-detail.component';
import { NgxSpinnerModule } from 'ngx-spinner';
import { FileUploadModule } from 'ng2-file-upload';
import { BsDatepickerModule } from 'ngx-bootstrap/datepicker';

@NgModule({
    declarations: [],
    imports: [
        CommonModule,
        MemberDetailComponent,
        BsDropdownModule.forRoot(),
        ToastrModule.forRoot({positionClass: 'toast-bottom-right'}),
        TabsModule.forRoot(),
        NgxSpinnerModule.forRoot({ type: 'line-scale-party' }),
        FileUploadModule,
        BsDatepickerModule.forRoot(),
    ],
    exports: [
        BsDropdownModule,
        ToastrModule,
        TabsModule,
        MemberDetailComponent,
        NgxSpinnerModule,
        FileUploadModule,
        BsDatepickerModule
    ]
})

export class SharedModule { }
