interface ISettings {
  title: string;
  showSettings: boolean;
  tagsView: boolean;
  fixedHeader: boolean;
  sidebarLogo: boolean;
  secondMenuPopup: boolean;
  errorLog: string | string[];
}

const settings:ISettings = {
  title: '골든바',

  showSettings: true,

  tagsView: true,

  fixedHeader: true,

  sidebarLogo: true,

  secondMenuPopup: false,

  errorLog: ['production', 'development'] 
};

export default settings;
