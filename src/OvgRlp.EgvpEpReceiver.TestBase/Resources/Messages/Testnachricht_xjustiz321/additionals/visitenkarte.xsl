<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:fo="http://www.w3.org/1999/XSL/Format">
  <xsl:output method="html" encoding="UTF-8" doctype-public="-//W3C//DTD HTML 4.0 Transitional//EN" cdata-section-elements="pre listing" indent="yes" />
  <xsl:template match="/">
    <html>
      <head>
        <title>businesscard.html</title>
        <style type="text/css" media="all">
          body { padding: 5px 20px 20px 20px; background: #ffffff; color: #000000; font-family: Verdana, sans-serif; font-size: 9px; } td { font-family: Verdana, sans-serif; font-size: 9px; } td.left { font-weight: bold; color:
          #808080; text-align: right; white-space: nowrap; } td.spacer { background-color: #ffffff; } h1 { font-weight: bold; font-size: 16px; } div.businesscard_section { background-color:
          #ffffff; width: 550px; border-width: 1px; border-style: solid; border-color: #808080; } div.sub { font-size: 8px; }
        </style>

      </head>
      <body>

        <h1>Visitenkarte</h1>
        <div class="businesscard_section">
          <table summary="">
            <tr>
              <td class="left">Nutzer-ID</td>
              <td>
                <xsl:value-of select="Visitenkarte/Nutzer_ID" />
              </td>
            </tr>
            <tr>
              <td class="left">Anrede</td>
              <td>
                <xsl:value-of select="Visitenkarte/Anrede" />
              </td>
            </tr>
            <tr>
              <td class="left">Akademischer Grad</td>
              <td>
                <xsl:value-of select="Visitenkarte/Titel" />
              </td>
            </tr>
            <tr>
              <td class="left">Name/Firma</td>
              <td>
                <xsl:value-of select="Visitenkarte/Name" />
              </td>
            </tr>            
            <tr>
              <td class="left">Vorname</td>
              <td>
                <xsl:value-of select="Visitenkarte/Vorname" />
              </td>
            </tr>
            <tr>
              <td class="left">Organisation</td>
              <td>
                <xsl:value-of select="Visitenkarte/Organisation" />
              </td>
            </tr>
            <tr>
              <td class="left">Organisationszusatz</td>
              <td>
                <xsl:value-of select="Visitenkarte/Organisationszusatz" />
              </td>
            </tr>
            <tr>
              <td class="left">Stra&#223;e</td>
              <td>
                <xsl:value-of select="Visitenkarte/Strasse" />
              </td>
            </tr>
            <tr>
              <td class="left">Hausnummer</td>
              <td>
                <xsl:value-of select="Visitenkarte/Hausnummer" />
              </td>
            </tr>            
            <tr>
              <td class="left">Postleitzahl</td>
              <td>
                <xsl:value-of select="Visitenkarte/Postleitzahl" />
              </td>
            </tr>
            <tr>
              <td class="left">Ort</td>
              <td>
                <xsl:value-of select="Visitenkarte/Ort" />
              </td>
            </tr>
            <tr>
              <td class="left">Bundesland</td>
              <td>
                <xsl:value-of select="Visitenkarte/Bundesland" />
              </td>
            </tr>             
            <tr>
              <td class="left">Land</td>
              <td>
                <xsl:value-of select="Visitenkarte/Land" />
              </td>
            </tr>
            <tr>
              <td class="left">E-Mail</td>
              <td>
                <xsl:value-of select="Visitenkarte/E-Mail" />
              </td>
            </tr>
            <tr>
              <td class="left">Mobiltelefon</td>
              <td>
                <xsl:value-of select="Visitenkarte/Mobiltelefon" />
              </td>
            </tr>            
            <tr>
              <td class="left">Telefon</td>
              <td>
                <xsl:value-of select="Visitenkarte/Telefon" />
              </td>
            </tr>
            <tr>
              <td class="left">Fax</td>
              <td>
                <xsl:value-of select="Visitenkarte/Fax" />
              </td>
            </tr>
          </table>
        </div>
      </body>
    </html>
  </xsl:template>
  <xsl:template match="@version" />
  <xsl:template match="Visitenkarte" />
</xsl:stylesheet>
