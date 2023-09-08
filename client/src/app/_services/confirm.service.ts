import { Injectable } from '@angular/core';
import { BsModalRef, BsModalService } from 'ngx-bootstrap/modal';
import { ConfirmDialogComponent } from '../modals/confirm-dialog/confirm-dialog.component';
import { Observable, map } from 'rxjs';

@Injectable({
    providedIn: 'root'
})
export class ConfirmService {
    bsModalRef?: BsModalRef<ConfirmDialogComponent>;

    constructor(private modalService: BsModalService) { }

    confirm(title = 'Confirmation', message = 'Are you sure you want to do this', btnOkText = 'Ok', btnCancelText = 'Cancel') : Observable<boolean> {
        const config = {
            initialState: {
                title: title,
                message: message,
                btnOkText: btnOkText,
                btnCancelText: btnCancelText
            }            
        }
        this.bsModalRef = this.modalService.show(ConfirmDialogComponent, config);

        return this.bsModalRef.onHidden!.pipe(
            map(() => {
                return this.bsModalRef!.content!.result;
            })
        )
    }
}
