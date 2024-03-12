# NLightning Bolts Noise

## About This Project

This project is a fork and significant modification of Noise.NET, originally developed by Nemanja Mijailovic and available under the MIT License. We have adapted and extended Noise.NET to suit our specific needs for enhanced functionality and integration capabilities.

The original Noise.NET project can be found here: [https://github.com/Metalnem/noise](https://github.com/Metalnem/noise).

## License

The modifications made in this specific folder of the project are licensed under the MIT License, consistent with the original licensing of Noise.NET. This ensures that both the original work and our modifications remain open and accessible for use, modification, and distribution by the community.

The rest of our project, outside of this specific folder, is licensed under a different license. Please refer to the corresponding [LICENSE](../../../../LICENSE) file located in the root of each folder for detailed licensing information.

A copy of the MIT License for the original Noise.NET project is included in the LICENSE file at the root of the modified project folder.

## Modifications

We have made several significant changes to the original Noise.NET project to better meet our needs, including:

- Replaced libsodium: We replaced libsodium in favor of using BouncyCastle.Cryptography in order to keep our dependencies to a bare minimun.

## Credits

- **[Níckolas Goline](https://github.com/ngoline)**: Initial work on the modifications and adaptation of the project.
- **[Nemanja Mijailovic](https://github.com/Metalnem)**: For the original Noise.NET project.

We extend our gratitude to all contributors to the original Noise.NET project and those who have contributed to this fork. Your efforts have laid the groundwork for further innovation and adaptation.