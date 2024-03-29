import { Component, OnInit } from '@angular/core';
import { BsModalRef } from 'ngx-bootstrap/modal';
import { ConfirmService } from 'src/app/_services/confirm.service';

@Component({
    selector: 'app-confirm-dialog',
    templateUrl: './confirm-dialog.component.html',
    styleUrls: ['./confirm-dialog.component.css']
})
export class ConfirmDialogComponent implements OnInit {
    title: string = '';
    message: string = '';
    btnOkText: string = '';
    btnCancelText: string = '';
    result: boolean = false;

    constructor(private bsModalRef: BsModalRef) { }
    
    ngOnInit(): void {
    }

    confirm() {
        this.result = true;
        this.bsModalRef.hide();
    }

    decline() { 
        this.bsModalRef.hide();
    }

}
