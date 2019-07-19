/**
* DevExtreme (ui/scroll_view.d.ts)
* Version: 18.2.8
* Build date: Tue Apr 23 2019
*
* Copyright (c) 2012 - 2019 Developer Express Inc. ALL RIGHTS RESERVED
* Read about DevExtreme licensing here: https://js.devexpress.com/Licensing/
*/
import DevExpress from '../bundles/dx.all';

declare global {
interface JQuery {
    dxScrollView(): JQuery;
    dxScrollView(options: "instance"): DevExpress.ui.dxScrollView;
    dxScrollView(options: string): any;
    dxScrollView(options: string, ...params: any[]): any;
    dxScrollView(options: DevExpress.ui.dxScrollViewOptions): JQuery;
}
}
export default DevExpress.ui.dxScrollView;
export type Options = DevExpress.ui.dxScrollViewOptions;

/** @deprecated use Options instead */
export type IOptions = DevExpress.ui.dxScrollViewOptions;