This is a "simple" implementation of Regions for SDL Tridion.

The documentation needs work...

Implementing Page Regions
To enable the proper usage of Experience Manager, we have decided to implement the concept of page regions, and tie Component Templates
to a given region.
Using regions we can show on experience manager visually what areas you can drag and drop your components to. Also the region constrains
regarding schema and component template pairs, allow us to change the presentation automatically(component template) when the component is
dropped into a different region. So the same content can be rendered differently, depending on the region where it is placed.
This gives us a limitation that a given Component Template can only be used within a given Region, but this was considered to be an acceptable
limitation.
You can tie a component to a Region by using the Component Template Metadata schema and selecting the keyword that defines your region.
Defining a region
If you need to define a new region, you can simply add a keyword to the Regions category.
This region keyword must use the "Region Settings" metadata schema, and you must define the region constraints that apply to it. In particular,
you must define the content types that are allowed within that region.
As can be seen above, the constraints have to be defined by using the schema and template's relative webdav url without including
/webdav/publication name. You can easily find an item's webdav Url by using the javascript console in chrome or firefox and typing the following
three commands:
var x = $models.getItem("tcm:1-2-3")
x.loadWebDavUrl()
x.getWebDavUrl()
When outputting a Page you can use the page extension method page.GetRegions() to retrieve a list of Regions in the page.
public static Dictionary<string, Region> GetRegions(this Page page)
The Region object is described below:
We have also developed a Dreamweaver custom function, "RenderRegion", that will render all component presentations within a given Region in
a page, and output the region markup as required by Experience Manager.
The Region Markup will only be outputted if the Publication Target is SiteEdit enabled.
Use this extension like this:
<div class="someclass">@@RenderRegion("Footer")@@</div>
Marking Component Templates to belong a Region
Component Templates are tagged to belong a Region using the "Component Template Metadata".
As mentioned earlier this has the limitation that a given Component Template can only be used within a given Region on the same page. The
workaround for this is to create a copy of the template. (Ángel: It is arguable how often we are actually going to use the same CT in different
Regions of the page. From the templates developed during the POC, this only happened once with the Columns Titles. )
1.
2.
a.
b.
3.
The benefits is that by limiting the Page Regions to only use one CT with each schema, we get the CT switched automatically when the
component is moved in Experience Manager, and by extension the CP is region is also switched automatically.
Extending a region
Obviously, different template themes will have different restrictions for what is essentially the same region. To cater for a scenario where, for
instance, you could have a footer with a different number of components, we create the "Theme" category. Essentially, these are the steps
required to override the "Footer" region for a theme named "Simple Theme".
Create a keyword in the "Themes" category and name it "Simple Theme". Do no use any metadata.
Create a child keyword of the "Simple Theme" keyword, and name it "Simple Theme - Footer".
Use the Region Settings Metadata Schema and define the constraints you have for the Footer region
Mark this child keyword as an abstract keyword
Open your Page Template and use the "Page Template Metadata" schema. Select "Simple Theme" as the theme for this page template.
The code in RenderRegion will find the correct region constraints to use. It is extremely important that you observe the syntax for overriding
regions in a theme. The code will try to find these region overrides by using a simple string compare on the keyword title, so be careful
with CASE and SPACES.
